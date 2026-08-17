import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalMensagem } from './modal-mensagem';

describe('ModalMensagem', () => {
  let component: ModalMensagem;
  let fixture: ComponentFixture<ModalMensagem>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModalMensagem]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModalMensagem);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
