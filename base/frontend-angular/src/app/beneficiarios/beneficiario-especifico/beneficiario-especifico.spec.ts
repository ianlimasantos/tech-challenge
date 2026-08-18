import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BeneficiarioEspecifico } from './beneficiario-especifico';

describe('BeneficiarioEspecifico', () => {
  let component: BeneficiarioEspecifico;
  let fixture: ComponentFixture<BeneficiarioEspecifico>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BeneficiarioEspecifico]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BeneficiarioEspecifico);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
