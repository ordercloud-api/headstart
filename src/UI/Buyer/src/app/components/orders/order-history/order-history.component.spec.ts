import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, convertToParamMap } from '@angular/router';

import { OrderHistoryComponent } from 'src/app/order/containers/order-history/order-history.component';
import { NgbPaginationModule, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { OcMeService } from '@ordercloud/angular-sdk';
import { OrderStatus } from 'src/app/order/models/order-status.model';
import { of, Subject } from 'rxjs';
import { take } from 'rxjs/operators';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('OrderHistoryComponent', () => {
  let component: OrderHistoryComponent;
  let fixture: ComponentFixture<OrderHistoryComponent>;

  const mockMe = { xp: { FavoriteOrders: ['a', 'b', 'c'] } };
  const meService = {
    ListOrders: jasmine.createSpy('ListOrders').and.returnValue(of(null)),
    Get: jasmine.createSpy('Get').and.returnValue(of(mockMe)),
    Patch: jasmine.createSpy('Patch').and.returnValue(of(mockMe)),
  };
  const router = { navigate: jasmine.createSpy('navigate') };
  const queryParamMap = new Subject<any>();
  const activatedRoute = {
    snapshot: {
      queryParams: {
        sortBy: '!ID',
        search: 'OrderID123',
        page: 1,
        status: 'Open',
        datesubmitted: ['5-30-18'],
      },
    },
    queryParamMap,
    queryParams: new Subject(),
  };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [OrderHistoryComponent],
      imports: [ReactiveFormsModule, NgbPaginationModule, NgbModule],
      providers: [
        { provide: OcMeService, useValue: meService },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
      ],
      schemas: [NO_ERRORS_SCHEMA], // Ignore template errors: remove if tests are added to test template
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OrderHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngAfterViewInit', () => {
    beforeEach(() => {
      spyOn(component as any, 'listOrders');
      component.ngAfterViewInit();
    });
    it('should call list orders', () => {
      expect(component['listOrders']).toHaveBeenCalled();
    });
  });

  describe('sortOrders', () => {
    it('should navigate to same route with updated sort params', () => {
      component['sortOrders']('Name');
      const queryParams = {
        ...activatedRoute.snapshot.queryParams,
        sortBy: 'Name',
      };
      expect(router.navigate).toHaveBeenCalledWith([], { queryParams });
    });
  });

  describe('changePage', () => {
    it('should navigate to same route with updated page params', () => {
      component['changePage'](2);
      const queryParams = { ...activatedRoute.snapshot.queryParams, page: 2 };
      expect(router.navigate).toHaveBeenCalledWith([], { queryParams });
    });
  });

  describe('filterBySearch', () => {
    it('should navigate to same route with updated search params', () => {
      component['filterBySearch']('another search term');
      const queryParams = {
        ...activatedRoute.snapshot.queryParams,
        search: 'another search term',
        page: undefined,
      };
      expect(router.navigate).toHaveBeenCalledWith([], { queryParams });
    });
  });

  describe('filterByStatus', () => {
    it('should navigate to same route with updated status params', () => {
      component['filterByStatus'](OrderStatus.Completed);
      const queryParams = {
        ...activatedRoute.snapshot.queryParams,
        status: OrderStatus.Completed,
      };
      expect(router.navigate).toHaveBeenCalledWith([], { queryParams });
    });
  });

  describe('filterByDate', () => {
    it('should navigate to same route with updated status params', () => {
      component['filterByDate'](['5-30-18']);
      const queryParams = {
        ...activatedRoute.snapshot.queryParams,
        datesubmitted: ['5-30-18'],
      };
      expect(router.navigate).toHaveBeenCalledWith([], { queryParams });
    });
  });

  describe('filterByFavorite', () => {
    beforeEach(() => {
      meService.ListOrders.calls.reset();
      spyOn(component as any, 'addQueryParam');
    });
    it('should show favorites only when true', () => {
      component['filterByFavorite'](true);
      expect(component['addQueryParam']).toHaveBeenCalledWith({
        favoriteOrders: true,
      });
    });
    it('should show all products when false', () => {
      component['filterByFavorite'](false);
      expect(component['addQueryParam']).toHaveBeenCalledWith({
        favoriteOrders: undefined,
      });
    });
  });

  describe('listOrders', () => {
    const expected = {
      sortBy: '!ID',
      search: 'OrderID123',
      page: 1,
      filters: {
        status: 'Open',
        datesubmitted: ['5-30-18'],
        ID: '1|2|3',
      },
    };
    beforeEach(() => {
      meService.ListOrders.calls.reset();
    });
    it('should call meService.ListOrders with correct parameters', () => {
      spyOn(component as any, 'buildFavoriteOrdersQuery').and.returnValue('1|2|3');
      component['listOrders']()
        .pipe(take(1))
        .subscribe(() => {
          expect(meService.ListOrders).toHaveBeenCalledWith(expected);
        });
      queryParamMap.next(convertToParamMap(activatedRoute.snapshot.queryParams));
    });
  });

  describe('buildFavoriteOrdersQuery', () => {
    describe('hasFavoriteOrdersFilter', () => {
      it('should be true if favoriteOrders param is string "true"', () => {
        component['buildFavoriteOrdersQuery'](convertToParamMap({ favoriteOrders: 'true' }));
        expect(component.hasFavoriteOrdersFilter).toBe(true);
      });
      it('should be false if favoriteOrders param is undefined', () => {
        component['buildFavoriteOrdersQuery'](convertToParamMap({ favoriteOrders: undefined }));
        expect(component.hasFavoriteOrdersFilter).toBe(false);
      });
      it('should be false if favoriteOrders param is null', () => {
        component['buildFavoriteOrdersQuery'](convertToParamMap({ favoriteOrders: null }));
        expect(component.hasFavoriteOrdersFilter).toBe(false);
      });
      it('should be false if favoriteOrders param does not exist', () => {
        component['buildFavoriteOrdersQuery'](convertToParamMap({ anotherParam: 'blah' }));
        expect(component.hasFavoriteOrdersFilter).toBe(false);
      });
    });
    describe('result', () => {
      const queryParamWithFavorites = convertToParamMap({
        favoriteOrders: 'true',
      });
      const queryParamWithoutFavorites = convertToParamMap({
        someParam: 'blah',
      });
    });
  });
});
