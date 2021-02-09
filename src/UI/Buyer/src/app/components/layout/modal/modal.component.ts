import {
  Component,
  ElementRef,
  Input,
  OnInit,
  OnDestroy,
  Inject,
  Output,
  EventEmitter,
} from '@angular/core'
import { faShoppingCart, faTimes } from '@fortawesome/free-solid-svg-icons'
import { DOCUMENT } from '@angular/common'

@Component({
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss'],
})
export class OCMModal implements OnInit, OnDestroy {
  @Input() modalTitle: string
  @Output() closeEvent = new EventEmitter()
  isOpen = false
  faTimes = faTimes
  faShoppingCart = faShoppingCart

  @Input() set state(value: { isOpen: boolean }) {
    if (value.isOpen && !this.isOpen) {
      this.openModal()
    } else if (!value.isOpen && this.isOpen) {
      this.closeModal()
    }
  }

  constructor(
    private elementRef: ElementRef,
    @Inject(DOCUMENT) private document: any
  ) {}

  ngOnInit(): void {}

  // remove self when directive is destroyed
  ngOnDestroy(): void {
    this.closeModal()
    this.elementRef.nativeElement.remove()
  }

  openModal(): void {
    this.isOpen = true
    this.elementRef.nativeElement.style.display = 'block'
    this.document.body.classList.add('shared-modal--open')
  }

  closeModal(): void {
    this.isOpen = false
    this.elementRef.nativeElement.style.display = 'none'
    this.document.body.classList.remove('shared-modal--open')
    this.closeEvent.emit()
  }
}
