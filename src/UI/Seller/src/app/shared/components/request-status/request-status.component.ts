import { Component, Input } from '@angular/core'

@Component({
  selector: 'request-status',
  templateUrl: './request-status.component.html',
  styleUrls: ['./request-status.component.scss'],
})
export class RequestStatus {
  @Input()
  requestStatus: RequestStatus
  @Input()
  subResourceName = ''
  @Input()
  selectedParentResouceName
}
