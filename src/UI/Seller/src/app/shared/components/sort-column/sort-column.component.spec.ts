import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'

import { NO_ERRORS_SCHEMA } from '@angular/core'
import { SortColumnComponent } from '@app-seller/shared/components/sort-column/sort-column.component'

describe('SortColumnComponent', () => {
  let component: SortColumnComponent
  let fixture: ComponentFixture<SortColumnComponent>

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [SortColumnComponent],
      //schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(SortColumnComponent)
    component = fixture.componentInstance
    spyOn(component.sort, 'emit')
    component.fieldName = 'ID'
    fixture.detectChanges()
  })

  it('should sort accending if first click', () => {
    component.currentSort = undefined
    component.changeSort()
    expect(component.sort.emit).toHaveBeenCalledWith('ID')
  })
  it('should sort descending if second click', () => {
    component.currentSort = 'ID'
    component.changeSort()
    expect(component.sort.emit).toHaveBeenCalledWith('!ID')
  })
  it('should clear sort if third click', () => {
    component.currentSort = '!ID'
    component.changeSort()
    expect(component.sort.emit).toHaveBeenCalledWith(undefined)
  })
})
