import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core'
import { DomSanitizer } from '@angular/platform-browser'
import { FileHandle } from '@app-seller/shared'
import { faTrash, faTimes } from '@fortawesome/free-solid-svg-icons'
import { NgbModal } from '@ng-bootstrap/ng-bootstrap'
import { DocumentAsset } from '@ordercloud/headstart-sdk'
import { DeleteFileEvent } from '../product-image-upload/product-image-upload.component'

@Component({
  selector: 'app-product-document-upload',
  templateUrl: './product-document-upload.component.html',
  styleUrls: ['./product-document-upload.component.scss'],
})
export class ProductDocumentUploadComponent {
  @Input() documents: DocumentAsset[]
  @Input() stagedDocuments: FileHandle[]
  @Output() stagedDocumentsChange = new EventEmitter<FileHandle[]>()
  @Output() deleteDocument = new EventEmitter<DeleteFileEvent>()

  faTrash = faTrash
  faTimes = faTimes

  documentName = ''

  constructor(
    private sanitizer: DomSanitizer,
    private modalService: NgbModal
  ) {}

  stageDocuments(documents: FileHandle[]): void {
    documents.forEach((document) => {
      const fileName = document.File.name.split('.')
      const ext = fileName[1]
      const fileNameWithExt = document.Filename + '.' + ext
      document.Filename = fileNameWithExt
    })
    const stagedDocuments = this.stagedDocuments.concat(documents)
    this.stagedDocumentsChange.emit(stagedDocuments)
  }

  unstageDocument(index: number): void {
    this.stagedDocuments.splice(index, 1)
    this.stagedDocumentsChange.emit(this.stagedDocuments)
  }

  removeDocument(file: DocumentAsset): void {
    this.deleteDocument.emit({ FileUrl: file.Url, AssetType: 'document' })
  }

  uploadDocument(event: Event & { target: HTMLInputElement }): void {
    const files: FileHandle[] = Array.from(event.target.files).map(
      (file: File) => {
        const URL = this.sanitizer.bypassSecurityTrustUrl(
          window.URL.createObjectURL(file)
        )
        return { File: file, URL, Filename: this.documentName }
      }
    )
    this.documentName = null
    this.stageDocuments(files)
  }

  getDocumentName(event: KeyboardEvent): void {
    this.documentName = (event.target as HTMLInputElement).value
  }

  openDeleteConfirmModal(content: unknown): void {
    this.modalService.open(content, { ariaLabelledBy: 'confirm-modal' })
  }
}
