import {
  Component,
  OnDestroy,
  Input,
  EventEmitter,
  Output,
  OnChanges,
  OnInit,
} from '@angular/core'
import { FormGroup, FormControl } from '@angular/forms'
import { faSearch, faTimes } from '@fortawesome/free-solid-svg-icons'
import { debounceTime, takeWhile, filter } from 'rxjs/operators'

@Component({
  selector: 'search-component',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss'],
})
export class SearchComponent implements OnInit, OnChanges, OnDestroy {
  alive = true
  @Input()
  placeholderText?: string
  @Input()
  searchTermInput = ''
  @Input()
  id = ''
  @Output()
  searched = new EventEmitter<string>()
  faSearch = faSearch
  faTimes = faTimes
  form: FormGroup
  previousSearchTerm = ''

  ngOnInit() {
    this.buildForm()
  }

  ngOnChanges() {
    this.previousSearchTerm = this.searchTermInput
  }

  buildForm() {
    this.form = new FormGroup({ search: new FormControl(this.searchTermInput) })
    this.onFormChanges()
  }

  private onFormChanges() {
    this.form.controls.search.valueChanges
      .pipe(
        filter((searchTerm) => {
          const userTriggered = this.form.dirty
          return searchTerm !== this.previousSearchTerm && userTriggered
        }),
        debounceTime(800),
        takeWhile(() => this.alive)
      )
      .subscribe((searchTerm) => {
        this.previousSearchTerm = searchTerm
        this.search()
      })
  }

  search() {
    this.form.markAsPristine()
    // emit as undefined if empty string so sdk ignores parameter completely
    this.searched.emit(this.getCurrentSearchTerm() || undefined)
  }

  getCurrentSearchTerm(): string {
    return this.form.get('search').value
  }

  showClear(): boolean {
    return this.getCurrentSearchTerm() !== ''
  }

  clear(): void {
    this.form.markAsDirty()
    this.form.setValue({ search: '' })
  }

  ngOnDestroy() {
    this.alive = false
  }
}
