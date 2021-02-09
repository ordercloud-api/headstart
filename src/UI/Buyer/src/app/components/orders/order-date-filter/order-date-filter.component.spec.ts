import { async, ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';

import { DateFilterComponent } from 'src/app/ocm-default-components/components/order-date-filter/order-date-filter.component';
import { ReactiveFormsModule } from '@angular/forms';
import { NgbDateParserFormatter, NgbDateAdapter, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { NgbDateNativeAdapter, NgbDateCustomParserFormatter } from 'src/app/config/date-picker.config';
import { DatePipe } from '@angular/common';

describe('DateFilterComponent', () => {
  let component: DateFilterComponent;
  let fixture: ComponentFixture<DateFilterComponent>;

  const formErrorService = { hasDateError: jasmine.createSpy('hasDateError') };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [DateFilterComponent, FaIconComponent],
      imports: [NgbModule, ReactiveFormsModule],
      providers: [
        DatePipe,
        { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter },
        {
          provide: NgbDateParserFormatter,
          useClass: NgbDateCustomParserFormatter,
        },
      ],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DateFilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    beforeEach(() => {
      spyOn(component as any, 'onFormChanges');
      component.ngOnInit();
    });
    it('should initialize form', () => {
      expect(component.form.value).toEqual({
        fromDate: null,
        toDate: null,
      });
    });
    it('should call onFormChanges', () => {
      expect(component['onFormChanges']).toHaveBeenCalled();
    });
  });

  describe('onFormChanges', () => {
    beforeEach(() => {
      spyOn(component as any, 'emitDate');
    });
    it('should call emitData after 500ms', fakeAsync(() => {
      component['onFormChanges']();
      component.form.controls['fromDate'].setValue('08-09-2018');
      tick(499);
      expect(component['emitDate']).not.toHaveBeenCalled();
      tick(1);
      expect(component['emitDate']).toHaveBeenCalled();
    }));
  });

  describe('emitDate', () => {
    beforeEach(() => {
      spyOn(component.selectedDate, 'emit');
    });
    it('should not run if fromDate is invalid', () => {
      component.form.controls['fromDate'].setErrors({ test: true });
      component['emitDate']();
      expect(component.selectedDate.emit).not.toHaveBeenCalled();
    });
    it('should not run if toDate is invalid', () => {
      component.form.controls['toDate'].setErrors({ test: true });
      component['emitDate']();
      expect(component.selectedDate.emit).not.toHaveBeenCalled();
    });
    it('should emit array with two values if fromDate and toDate are defined', () => {
      component.form.controls['fromDate'].setValue(new Date(2018, 4, 20));
      component.form.controls['toDate'].setValue(new Date(2018, 4, 31));
      component['emitDate']();
      expect(component.selectedDate.emit).toHaveBeenCalledWith(['>5-20-18', '<6-1-18']);
    });
    it('should emit array with fromDate if only fromDate is defined', () => {
      component.form.controls['fromDate'].setValue(new Date(2018, 4, 20));
      component['emitDate']();
      expect(component.selectedDate.emit).toHaveBeenCalledWith(['>5-20-18']);
    });
    it('should emit array with toDate if only toDate is defined', () => {
      component.form.controls['toDate'].setValue(new Date(2018, 4, 31));
      component['emitDate']();
      expect(component.selectedDate.emit).toHaveBeenCalledWith(['<6-1-18']);
    });
  });

  describe('clearToDate', () => {
    beforeEach(() => {
      spyOn(component.selectedDate, 'emit');
    });
    it('should clear to to Date and emit', () => {
      component.form.controls['fromDate'].setValue(new Date(2018, 4, 20));
      component.form.controls['toDate'].setValue(new Date(2018, 4, 31));
      component['clearToDate']();
      expect(component.selectedDate.emit).toHaveBeenCalledWith(['>5-20-18']);
    });
  });

  describe('clearFromDate', () => {
    beforeEach(() => {
      spyOn(component.selectedDate, 'emit');
    });
    it('should clear to to Date and emit', () => {
      component.form.controls['fromDate'].setValue(new Date(2018, 4, 20));
      component.form.controls['toDate'].setValue(new Date(2018, 4, 31));
      component['clearFromDate']();
      expect(component.selectedDate.emit).toHaveBeenCalledWith(['<6-1-18']);
    });
  });

  describe('ngOnDestroy', () => {
    it('should set alive to false', () => {
      expect(component['alive']).toBe(true);
      component.ngOnDestroy();
      expect(component['alive']).toBe(false);
    });
  });
});
