import {
  Component,
  Input,
  OnInit,
} from '@angular/core'
import { ReflektionSearchResponse } from 'src/app/services/reflektion/models/ReflektionSearchResponse'

@Component({
  templateUrl: './preview-search.component.html',
  styleUrls: ['./preview-search.component.scss'],
})
export class OCMPreviewSearch implements OnInit {
  @Input() searchTerm?: string;
  @Input() results?: ReflektionSearchResponse;

  ngOnInit(): void {
  }

}
