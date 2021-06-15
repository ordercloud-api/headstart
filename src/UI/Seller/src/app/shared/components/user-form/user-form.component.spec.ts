import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { AppFormErrorService } from '@app-seller/shared';
import { UserFormComponent } from '@app-seller/shared/components/user-form/user-form.component';

describe('UserFormComponent', () => {
  let component: UserFormComponent;
  let fixture: ComponentFixture<UserFormComponent>;

  const mockUser = {
    ID: '1',
    Username: 'Products',
    FirstName: 'First',
    LastName: 'Second',
    Email: 'test@email.com',
    Phone: '123-456-7890',
    Active: true,
  };

  const formErrorService = {
    hasRequiredError: jasmine.createSpy('hasRequiredError'),
    displayFormErrors: jasmine.createSpy('displayFormErrors'),
    hasValidEmailError: jasmine.createSpy('hasValidEmailError'),
    hasPatternError: jasmine.createSpy('hasPatternError'),
  };

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [UserFormComponent],
      imports: [ReactiveFormsModule],
      providers: [
        FormBuilder,
        { provide: AppFormErrorService, useValue: formErrorService },
      ],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserFormComponent);
    component = fixture.componentInstance;
    component.existingUser = mockUser;
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
      expect(component.userForm.value).toEqual({
        ID: '1',
        Username: 'Products',
        FirstName: 'First',
        LastName: 'Second',
        Email: 'test@email.com',
        Phone: '123-456-7890',
        Active: true,
      });
    });
  });

  describe('onSubmit', () => {
    beforeEach(() => {
      spyOn(component.formSubmitted, 'emit');
    });
    it('should call displayFormErrors if form is invalid', () => {
      component.userForm.setErrors({ test: true });
      component['onSubmit']();
      expect(formErrorService.displayFormErrors).toHaveBeenCalled();
    });
    it('should emit formSubmitted event', () => {
      component.userForm.setErrors(null);
      component.userForm.get('ID').setValue('newID');
      component['onSubmit']();
      expect(component.formSubmitted.emit).toHaveBeenCalledWith({
        user: {
          ID: 'newID',
          Username: 'Products',
          FirstName: 'First',
          LastName: 'Second',
          Email: 'test@email.com',
          Phone: '123-456-7890',
          Active: true,
        },
        prevID: '1',
      });
    });
  });

  describe('hasRequiredError', () => {
    beforeEach(() => {
      component['hasRequiredError']('FirstName');
    });
    it('should call formErrorService.hasRequiredError', () => {
      expect(formErrorService.hasRequiredError).toHaveBeenCalledWith(
        'FirstName',
        component.userForm
      );
    });
  });
});
