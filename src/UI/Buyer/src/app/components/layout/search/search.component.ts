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
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss'],
})
export class OCMSearch implements OnInit, OnChanges, OnDestroy {
  @Input() placeholderText?: string
  @Input() searchTermInput?: string
  @Output() searched = new EventEmitter<string>()

  alive = true
  faSearch = faSearch
  faTimes = faTimes
  form: FormGroup
  previousSearchTerm = ''

  ngOnInit(): void {
    this.buildForm()
  }

  ngOnChanges(): void {
    if (this.searchTermInput !== null && this.searchTermInput !== undefined) {
      this.previousSearchTerm = this.searchTermInput
    }
  }

  buildForm(): void {
    this.form = new FormGroup({ search: new FormControl('') })
    this.onFormChanges()
  }

  search(): void {
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

  ngOnDestroy(): void {
    this.alive = false
  }

  private onFormChanges(): void {
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
}
