import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalErro } from './modal-erro';

describe('ModalErro', () => {
  let component: ModalErro;
  let fixture: ComponentFixture<ModalErro>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModalErro]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModalErro);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
