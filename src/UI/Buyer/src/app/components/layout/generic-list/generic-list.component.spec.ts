import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GenericBrowseComponent } from 'src/app/shared/components/generic-browse/generic-browse.component';
import { Directive, Input, Output, EventEmitter, NO_ERRORS_SCHEMA } from '@angular/core';
import { By } from '@angular/platform-browser';

describe('GenericBrowseComponent', () => {
  let component: GenericBrowseComponent<any>;
  let fixture: ComponentFixture<GenericBrowseComponent<any>>;

  /* tslint:disable: directive-selector */
  @Directive({
    selector: 'shared-search',
  })
  class MockSearchDirective {
    @Input() placeholderText: string;
    @Output() searched = new EventEmitter<string>();
  }

  @Directive({
    selector: 'ngb-pagination',
  })
  class MockPaginationDirective {
    @Input() collectionSize: number;
    @Input() pageSize: number;
    @Input() page: number;
    @Output() pageChange = new EventEmitter<number>();
  }
  /* tslint:enable: directive-selector */

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [GenericBrowseComponent, MockSearchDirective, MockPaginationDirective],
      schemas: [NO_ERRORS_SCHEMA], // Ignore template errors: remove if tests are added to test template
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GenericBrowseComponent);
    fixture.autoDetectChanges();
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('on child component events', () => {
    beforeEach(() => {
      component.meta = { TotalPages: 2 };
      spyOn(component.requestOptionsUpdated, 'emit');
      fixture.detectChanges();
    });

    it('should re-emit the search term', () => {
      const mocksearchEl = fixture.debugElement.query(By.directive(MockSearchDirective));
      const mockSearchDir = mocksearchEl.injector.get(MockSearchDirective) as MockSearchDirective;

      mockSearchDir.searched.emit('searchTerm');

      expect(component.requestOptionsUpdated.emit).toHaveBeenCalledWith({
        search: 'searchTerm',
        page: undefined,
      });
    });

    it('should re-emit the page number', () => {
      const mockPageEl = fixture.debugElement.query(By.directive(MockPaginationDirective));
      const mockPageDir = mockPageEl.injector.get(MockPaginationDirective) as MockPaginationDirective;

      mockPageDir.pageChange.emit(5);

      expect(component.requestOptionsUpdated.emit).toHaveBeenCalledWith({
        page: 5,
      });
    });
  });
});
