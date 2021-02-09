import { Injectable } from '@angular/core'
import domtoimage from 'dom-to-image'
import html2canvas from 'html2canvas'
import * as jsPDF from 'jspdf'

@Injectable({
  providedIn: 'root',
})
export class PDFService {
  html: string

  async createAndSavePDF(orderID: string) {
    /*
    Code taken largely from what Winmark uses for the Order Invoice PDF Download.
    There are different functions for whether or not the user is on Safari.
    */

    const orderDetailObject = document
      .getElementsByClassName('order-detail-pdf-range')[0]
      .cloneNode(true)

    // removes all of the elements that are marked with d-print-none
    this.removeNodesOfClass(orderDetailObject, 'd-print-none')

    const pdfArea = document.createElement('div')
    pdfArea.classList.add('hidden-print-area')
    pdfArea.appendChild(orderDetailObject)
    // const imgElement = <HTMLElement>pdfArea.querySelector('img');
    // imgElement.style.margin = "1em 40px";
    const links = pdfArea.getElementsByClassName('link-text')
    Array.from(links).forEach((link) => {
      link.classList.add('pdf-links')
    })
    document.body.appendChild(pdfArea)
    const printObj = document.getElementsByClassName(
      'order-detail-pdf-range'
    )[1]
    this.generateImagePDF(orderDetailObject, printObj, orderID)
  }

  private generateImagePDF(orderDetailObject, printObj, orderID) {
    domtoimage
      .toPng(printObj)
      .then((dataUrl) => {
        const img = new Image()
        img.src = dataUrl
        img.onload = () => {
          const width = img.width
          const height = img.height
          const aspectRatio = width / height
          const pdfHeight = 170 / aspectRatio
          const pdf = new jsPDF()
          pdf.addImage(dataUrl, 'PNG', 20, 20, 170, 170 / aspectRatio)
          const pages = Math.ceil(pdfHeight / 300)
          if (pages > 1) {
            let prevPageHeight = 0
            for (let i = 1; i < pages; i++) {
              const newY =
                i == 1
                  ? 20 - pdf.internal.pageSize.height
                  : prevPageHeight - pdf.internal.pageSize.height
              pdf.addPage()
              pdf.addImage(dataUrl, 'PNG', 20, newY, 170, pdfHeight)
              prevPageHeight = newY
            }
          }
          pdf.save(orderID + '.pdf')

          /* this removes from the dom the hidden print area
           this prevents the next pdf download from creating a
           pdf with the old information in the dom */
          this.removeNodesOfClass(document, 'hidden-print-area')
        }
      })
      .catch(
        //if promise does not resolve, generate the pdf with html2canvas and don't use an image
        (e) => {
          console.log(e)
          this.generateNoImagePDF(orderDetailObject, printObj, orderID)
        }
      )
  }

  private async generateNoImagePDF(orderDetailObject, printObj, orderID) {
    try {
      this.removeNodesOfClass(orderDetailObject, 'img-thumbnail')
      const canvas = await html2canvas(printObj, { allowTaint: true })
      const pdf = new jsPDF()
      const width = canvas.width
      const height = canvas.height
      const aspectRatio = width / height
      const imgData = canvas.toDataURL('image/png')
      pdf.addImage(imgData, 'PNG', 20, 20, 170, 170 / aspectRatio)
      pdf.save(orderID + '.pdf')
    } finally {
      this.removeNodesOfClass(document, 'hidden-print-area')
    }

  }

  private removeNodesOfClass(parentObject: any, classToRemove: string): void {
    if (parentObject && parentObject.childNodes) {
      parentObject.childNodes.forEach((childNode) => {
        if (
          childNode['classList'] &&
          Array.from(childNode['classList']).includes(classToRemove)
        ) {
          parentObject.removeChild(childNode)
        } else {
          this.removeNodesOfClass(childNode, classToRemove)
        }
      })
    }
  }
}
