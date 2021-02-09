import { async, ComponentFixture, TestBed } from '@angular/core/testing'

import { CategoryDetailsComponent } from './category-details.component'
import { NO_ERRORS_SCHEMA } from '@angular/core'
import { RouterTestingModule } from '@angular/router/testing'
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome'
import { OcUserGroupService, OcCategoryService } from '@ordercloud/angular-sdk'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { ActivatedRoute } from '@angular/router'
import { BehaviorSubject, of } from 'rxjs'

describe('CategoryDetailsComponent', () => {
  let component: CategoryDetailsComponent
  let fixture: ComponentFixture<CategoryDetailsComponent>
  const mockCategory = { ID: 'myCategoryID' }
  const ocCategoryService = {
    getParentResourceID() {
      return 1
    },
    Get: jasmine.createSpy('Get').and.returnValue(of({})),
    Patch: jasmine.createSpy('Patch').and.returnValue(of(mockCategory)),
  }
  const activatedRoute = {
    params: new BehaviorSubject({ categoryID: 'myCategoryID' }),
  }

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [CategoryDetailsComponent],
      imports: [RouterTestingModule, FontAwesomeModule],
      providers: [
        { provide: OcCategoryService, useValue: ocCategoryService },
        { provide: applicationConfiguration, useValue: { buyerID: 'buyerID' } },
        { provide: ActivatedRoute, useValue: activatedRoute },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(CategoryDetailsComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })

  describe('ngOnInit', () => {
    beforeEach(() => {
      spyOn(component, 'getCategoryData').and.returnValue(of(mockCategory))
      component.ngOnInit()
    })
    it('should set category', () => {
      expect(component.getCategoryData).toHaveBeenCalled()
    })
  })

  describe('GetCategoryData', () => {
    it('should call OcCategoryService and set categoryID', () => {
      spyOn(component, 'getCategoryData').and.returnValue(of(mockCategory))
      component.getCategoryData()
      expect(component.categoryID).toEqual(mockCategory.ID)
      expect(ocCategoryService.Get).toHaveBeenCalled()
    })
  })

  describe('updateProduct', () => {
    it('should update using existing categoryID', () => {
      const mock = { ID: 'newID' }
      component.updateCategory(mock)
      expect(ocCategoryService.Patch).toHaveBeenCalledWith(
        component.catalogID,
        component.categoryID,
        mock
      )
    })
  })
})
