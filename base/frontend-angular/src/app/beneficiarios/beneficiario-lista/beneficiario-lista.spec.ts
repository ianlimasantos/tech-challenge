import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BeneficiarioLista } from './beneficiario-lista';

describe('BeneficiarioLista', () => {
  let component: BeneficiarioLista;
  let fixture: ComponentFixture<BeneficiarioLista>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BeneficiarioLista]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BeneficiarioLista);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
