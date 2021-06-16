import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'

import { OrderListComponent } from 'src/app/ocm-default-components/components/order-list/order-list.component'
import { FaIconComponent } from '@fortawesome/angular-fontawesome'
import {
  NgbPaginationModule,
  NgbDateAdapter,
  NgbDateParserFormatter,
} from '@ng-bootstrap/ng-bootstrap'
import {
  NgbDateNativeAdapter,
  NgbDateCustomParserFormatter,
} from 'src/app/config/date-picker.config'
import { NO_ERRORS_SCHEMA } from '@angular/core'

describe('OrderListComponent', () => {
  let component: OrderListComponent
  let fixture: ComponentFixture<OrderListComponent>

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [FaIconComponent, OrderListComponent],
      imports: [NgbPaginationModule],
      providers: [
        { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter },
        {
          provide: NgbDateParserFormatter,
          useClass: NgbDateCustomParserFormatter,
        },
      ],
      schemas: [NO_ERRORS_SCHEMA], // Ignore template errors: remove if tests are added to test template
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(OrderListComponent)
    component = fixture.componentInstance
    component.orders = {
      Items: [],
      Meta: { TotalCount: 0, TotalPages: 0, Page: 1, PageSize: 25 },
    }
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })

  describe('updateSort', () => {
    beforeEach(() => {
      spyOn(component.updatedSort, 'emit')
    })
    it('should emit negative filter if sortBy is set to value', () => {
      component.sortBy = 'ID'
      component['updateSort']('ID')
      expect(component.updatedSort.emit).toHaveBeenCalledWith('!ID')
    })
    it('should emit undefined if sortBy is set to negative value', () => {
      component.sortBy = '!ID'
      component['updateSort']('ID')
      expect(component.updatedSort.emit).toHaveBeenCalledWith(undefined)
    })
    it('should emit value if passed in value neither matches sortBy or is a negation of sortBy', () => {
      component.sortBy = 'SomethingElse'
      component['updateSort']('ID')
      expect(component.updatedSort.emit).toHaveBeenCalledWith('ID')
    })
    it('should emit value if sortBy is null', () => {
      component.sortBy = null
      component['updateSort']('ID')
      expect(component.updatedSort.emit).toHaveBeenCalledWith('ID')
    })
  })

  describe('changePage', () => {
    beforeEach(() => {
      spyOn(component.changedPage, 'emit')
      component['changePage'](2)
    })
    it('should emit passed in page', () => {
      expect(component.changedPage.emit).toHaveBeenCalledWith(2)
    })
  })
})
