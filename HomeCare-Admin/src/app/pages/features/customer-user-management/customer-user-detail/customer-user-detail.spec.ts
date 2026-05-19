import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomerUserDetail } from './customer-user-detail';

describe('CustomerUserDetail', () => {
  let component: CustomerUserDetail;
  let fixture: ComponentFixture<CustomerUserDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CustomerUserDetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CustomerUserDetail);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
