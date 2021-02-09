import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { AddressFormComponent } from 'src/app/shared/components/address-form/address-form.component';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { of } from 'rxjs';
import { OcMeService } from '@ordercloud/angular-sdk';

describe('AddressFormComponent', () => {
  let component: AddressFormComponent;
  let fixture: ComponentFixture<AddressFormComponent>;
  const meService = {
    CreateAddress: jasmine.createSpy('CreateAddress').and.returnValue(of(null)),
    PatchAddress: jasmine.createSpy('PatchAddress').and.returnValue(of(null)),
  };
  const formErrorService = {
    hasRequiredError: jasmine.createSpy('hasRequiredError'),
    displayFormErrors: jasmine.createSpy('displayFormErrors'),
    hasPatternError: jasmine.createSpy('hasPatternError'),
  };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [AddressFormComponent],
      imports: [ReactiveFormsModule],
      providers: [FormBuilder, { provide: OcMeService, useValue: meService }],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddressFormComponent);
    component = fixture.componentInstance;
    component.existingAddress = {
      ID: '',
      FirstName: 'Crhistian',
      LastName: 'Ramirez',
      Street1: '404 5th st sw',
      Street2: null,
      City: 'Minneapolis',
      State: 'MN',
      Zip: '56001',
      Phone: '555-555-5555',
      Country: 'US',
    };
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should initialize form correctly', () => {
      expect(component.addressForm.value).toEqual({
        ID: '',
        FirstName: 'Crhistian',
        LastName: 'Ramirez',
        Street1: '404 5th st sw',
        Street2: '',
        City: 'Minneapolis',
        State: 'MN',
        Zip: '56001',
        Phone: '555-555-5555',
        Country: 'US',
      });
    });
  });

  describe('onSubmit', () => {
    beforeEach(() => {
      spyOn(component.formSubmitted, 'emit');
    });
    it('should call displayFormErrors if form is invalid', () => {
      component.addressForm.setErrors({ test: true });
      component['onSubmit']();
      expect(formErrorService.displayFormErrors).toHaveBeenCalled();
    });
    it('should emit formSubmitted event', () => {
      component['onSubmit']();
      expect(component.formSubmitted.emit).toHaveBeenCalledWith({
        address: {
          ID: '',
          FirstName: 'Crhistian',
          LastName: 'Ramirez',
          Street1: '404 5th st sw',
          Street2: '',
          City: 'Minneapolis',
          State: 'MN',
          Zip: '56001',
          Phone: '555-555-5555',
          Country: 'US',
        },
        formDirty: component.addressForm.dirty,
      });
    });
  });

  describe('hasRequiredError', () => {
    beforeEach(() => {
      component['hasRequiredError']('FirstName');
    });
    it('should call formErrorService.hasRequiredError', () => {
      expect(formErrorService.hasRequiredError).toHaveBeenCalledWith('FirstName', component.addressForm);
    });
  });
});
