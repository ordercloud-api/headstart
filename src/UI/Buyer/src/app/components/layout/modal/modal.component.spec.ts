import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalComponent } from 'src/app/shared/components/modal/modal.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { Subject } from 'rxjs';

describe('SharedModalComponent', () => {
  let component: ModalComponent;
  let fixture: ComponentFixture<ModalComponent>;

  const modalService = {
    add: jasmine.createSpy('add'),
    remove: jasmine.createSpy('remove'),
    onCloseSubject: new Subject<string>(),
  };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ModalComponent],
      imports: [FontAwesomeModule],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ModalComponent);
    component = fixture.componentInstance;
    component.id = 'ID';
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should throw an error if id is not defined', () => {
      component.id = null;
      expect(() => {
        component.ngOnInit();
      }).toThrow(new Error('modal must have an id'));
    });
    it('should call modalService add()', () => {
      component.ngOnInit();
      expect(modalService.add).toHaveBeenCalledWith(component);
    });
  });

  describe('open', () => {
    it('should make the modal visable', () => {
      component.open();
      expect(component.isOpen).toEqual(true);
      expect(document.body.classList).toContain('shared-modal--open');
    });
  });
  describe('close', () => {
    it('should make the modal not visable', () => {
      spyOn(modalService.onCloseSubject, 'next');
      component.close();
      expect(component.isOpen).toEqual(false);
      expect(document.body.classList).not.toContain('shared-modal--open');
      expect(modalService.onCloseSubject.next).toHaveBeenCalledWith(component.id);
    });
  });
  describe('ngOnDestroy', () => {
    beforeEach(() => {
      spyOn(component, 'close');
      component.ngOnDestroy();
    });
    it('should call close()', () => {
      expect(component.close).toHaveBeenCalled();
    });
    it('should call modalService remove', () => {
      expect(modalService.remove).toHaveBeenCalledWith(component.id);
    });
  });
});
