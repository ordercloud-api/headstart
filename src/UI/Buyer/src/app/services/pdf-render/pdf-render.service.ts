import { Injectable } from '@angular/core'
import domtoimage from 'dom-to-image'
import html2canvas from 'html2canvas'
import jspdf from 'jspdf'

@Injectable({
  providedIn: 'root',
})
export class PDFService {
  html: string

  constructor() {}

  createAndSavePDF(orderID: string): void {
    const printObj = document.getElementsByClassName(
      'order-detail-pdf-range'
    )[0]
    this.generateImagePDF(printObj, orderID)
  }

  private generateImagePDF(printObj: any, orderID: string): void {
    domtoimage
      .toPng(printObj, {
        filter: (node: Node) =>
          (node instanceof Element &&
            !node.classList.contains('d-print-none')) ||
          !(node instanceof Element),
      })
      .then((dataUrl) => {
        const img = new Image()
        img.src = dataUrl
        img.onload = (): void => {
          const width = img.width
          const height = img.height
          const aspectRatio = width / height
          const pdfHeight = 170 / aspectRatio
          const pdf = new jspdf()
          pdf.addImage(dataUrl, 'PNG', 20, 20, 170, pdfHeight)
          const pages = Math.ceil(pdfHeight / 300)
          if (pages > 1) {
            let prevPageHeight = 0
            for (let i = 1; i < pages; i++) {
              const newY =
                i === 1
                  ? 20 - pdf.internal.pageSize.height
                  : prevPageHeight - pdf.internal.pageSize.height
              pdf.addPage()
              pdf.addImage(dataUrl, 'PNG', 20, newY, 170, pdfHeight)
              prevPageHeight = newY
            }
          }
          pdf.save(orderID + '.pdf')
          this.removeNodesOfClass(document, 'hidden-print-area')
        }
      })
      .catch((e) => {
        this.generateNoImagePDF(printObj, orderID)
      })
  }

  private async generateNoImagePDF(
    printObj: any,
    orderID: string
  ): Promise<void> {
    this.removeNodesOfClass(printObj, 'img-thumbnail')
    const canvas = await html2canvas(printObj, { allowTaint: true })
    const pdf = new jspdf()
    const width = canvas.width
    const height = canvas.height
    const aspectRatio = width / height
    const imgData = canvas.toDataURL('image/png')
    pdf.addImage(imgData, 'PNG', 20, 20, 170, 170 / aspectRatio)
    pdf.save(orderID + '.pdf')
    this.removeNodesOfClass(document, 'hidden-print-area')
  }

  private removeNodesOfClass(parentObject: any, classToRemove: string): void {
    if (parentObject && parentObject.childNodes) {
      parentObject.childNodes.forEach((childNode) => {
        if (
          childNode.classList &&
          Array.from(childNode.classList).includes(classToRemove)
        ) {
          parentObject.removeChild(childNode)
        } else {
          this.removeNodesOfClass(childNode, classToRemove)
        }
      })
    }
  }
}
