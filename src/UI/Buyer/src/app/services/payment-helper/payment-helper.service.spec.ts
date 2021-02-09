import { async, TestBed, inject } from '@angular/core/testing';

import { AppPaymentService } from 'src/app/shared/services/payment-helper/payment-helper.service';
import { of } from 'rxjs';
import { OcMeService, OcPaymentService } from '@ordercloud/angular-sdk';

describe('AppPaymentService', () => {
  const meService = {
    GetCreditCard: jasmine.createSpy('GetCreditCard').and.callFake((cc) => of(cc)),
    GetSpendingAccount: jasmine.createSpy('GetSpendingAccount').and.callFake((sa) => of(sa)),
  };
  const paymentList = {
    Items: [
      { Type: 'CreditCard', CreditCardID: 'CreditCardOne' },
      { Type: 'CreditCard', CreditCardID: 'CreditCardTwo' },
      { Type: 'SpendingAccount', SpendingAccountID: 'SpendingAccountOne' },
      { Type: 'SpendingAccount', SpendingAccountID: 'SpendingAccountTwo' },
      {
        Type: 'PurchaseOrder',
        ID: 'PurchaseOrderOne',
        xp: { PONumber: '123456' },
      },
    ],
  };
  const paymentService = {
    List: jasmine.createSpy('List').and.returnValue(of(paymentList)),
  };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      providers: [{ provide: OcMeService, useValue: meService }, { provide: OcPaymentService, useValue: paymentService }],
    });
  }));

  it('should be created', inject([AppPaymentService], (service: AppPaymentService) => {
    expect(service).toBeTruthy();
  }));

  it('should call paymentService.list', inject([AppPaymentService], (service: AppPaymentService) => {
    service.ListPaymentsOnOrder('outgoing', 'ID');
    expect(paymentService.List).toHaveBeenCalled();
  }));
  it('should call meService.GetCredit card for each cc payment', inject([AppPaymentService], (service: AppPaymentService) => {
    meService.GetCreditCard.calls.reset();
    service.ListPaymentsOnOrder('outgoing', 'ID');
    expect(meService.GetCreditCard).toHaveBeenCalledWith('CreditCardOne');
    expect(meService.GetCreditCard).toHaveBeenCalledWith('CreditCardTwo');
    expect(meService.GetCreditCard.calls.count()).toBe(2);
  }));
  it('should call meService.GetSpendingAccount', inject([AppPaymentService], (service: AppPaymentService) => {
    meService.GetSpendingAccount.calls.reset();
    service.ListPaymentsOnOrder('outgoing', 'ID');
    expect(meService.GetSpendingAccount).toHaveBeenCalledWith('SpendingAccountOne');
    expect(meService.GetSpendingAccount).toHaveBeenCalledWith('SpendingAccountTwo');
    expect(meService.GetSpendingAccount.calls.count()).toBe(2);
  }));
});
