import { Component, Input } from '@angular/core'

@Component({
  templateUrl: './confirm-modal.component.html',
  styleUrls: ['./confirm-modal.component.scss'],
})
export class ConfirmModal {
  @Input()
  modalTitle: string
  @Input()
  description: string
}
