import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalExcluir } from './modal-excluir';

describe('ModalExcluir', () => {
  let component: ModalExcluir;
  let fixture: ComponentFixture<ModalExcluir>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModalExcluir]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModalExcluir);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
