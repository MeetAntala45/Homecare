import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PartnerLeave } from './partner-leave';

describe('PartnerLeave', () => {
  let component: PartnerLeave;
  let fixture: ComponentFixture<PartnerLeave>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PartnerLeave]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PartnerLeave);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
