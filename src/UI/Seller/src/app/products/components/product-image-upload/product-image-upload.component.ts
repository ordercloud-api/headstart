import { Component, Input, Output, EventEmitter } from '@angular/core'
import { DomSanitizer } from '@angular/platform-browser'
import { FileHandle } from '@app-seller/shared'
import { faTimes, faTrash } from '@fortawesome/free-solid-svg-icons'
import { NgbModal } from '@ng-bootstrap/ng-bootstrap'
import { AssetType, DocumentAsset, ImageAsset } from '@ordercloud/headstart-sdk'

export interface DeleteFileEvent {
  FileUrl: string
  AssetType: AssetType
}

@Component({
  selector: 'app-product-image-upload',
  templateUrl: './product-image-upload.component.html',
  styleUrls: ['./product-image-upload.component.scss'],
})
export class ProductImageUploadComponent {
  @Input() images: ImageAsset[]
  @Input() stagedImages: FileHandle[]
  @Output() stagedImagesChange = new EventEmitter<FileHandle[]>()
  @Output() deleteImage = new EventEmitter<DeleteFileEvent>()

  faTrash = faTrash
  faTimes = faTimes

  constructor(
    private sanitizer: DomSanitizer,
    private modalService: NgbModal
  ) {}

  stageImages(files: FileHandle[]): void {
    const stagedImages = this.stagedImages.concat(files)
    this.stagedImagesChange.emit(stagedImages)
  }

  unstageImage(index: number): void {
    this.stagedImages.splice(index, 1)
    this.stagedImagesChange.emit(this.stagedImages)
  }

  removeImage(file: DocumentAsset): void {
    this.deleteImage.emit({ FileUrl: file.Url, AssetType: 'image' })
  }

  uploadImage(event: Event & { target: HTMLInputElement }): void {
    const files: FileHandle[] = Array.from(event.target.files).map(
      (file: File) => {
        const URL = this.sanitizer.bypassSecurityTrustUrl(
          window.URL.createObjectURL(file)
        )
        return { File: file, URL }
      }
    )
    this.stageImages(files)
  }

  openDeleteConfirmModal(content: unknown): void {
    this.modalService.open(content, { ariaLabelledBy: 'confirm-modal' })
  }
}
