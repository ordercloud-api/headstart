import {
  ComponentFactoryResolver,
  Directive,
  Input,
  ViewContainerRef,
  OnInit,
  ComponentRef,
  OnChanges,
} from '@angular/core'
import { FormGroup } from '@angular/forms'
import { Field, FieldConfig } from 'src/app/models/product.types'
import { specFormComponents } from './components'

@Directive({
  selector: '[ocSpecField]',
})
export class SpecFieldDirective implements Field, OnChanges, OnInit {
  @Input() config: FieldConfig
  @Input() group: FormGroup
  @Input() index: number
  @Input() compact?: boolean = false
  component: ComponentRef<Field>
  components = specFormComponents

  constructor(
    private resolver: ComponentFactoryResolver,
    private container: ViewContainerRef
  ) {}

  ngOnChanges(): void {
    if (this.component) {
      this.component.instance.config = this.config
      this.component.instance.group = this.group
      this.component.instance.index = this.index
      this.component.instance.compact = this.compact
    }
  }

  ngOnInit(): void {
    if (!this.components[this.config.type]) {
      const supportedTypes = Object.keys(this.components).join(', ')
      throw new Error(
        `Trying to use an unsupported type (${this.config.type}).
        Supported types: ${supportedTypes}`
      )
    }
    const component = this.resolver.resolveComponentFactory<Field>(
      this.components[this.config.type]
    )
    this.component = this.container.createComponent(component)
    this.component.instance.config = this.config
    this.component.instance.group = this.group
    this.component.instance.index = this.index
    this.component.instance.compact = this.compact
  }
}
