import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AllNotificationDialog } from './all-notification-dialog';

describe('AllNotificationDialog', () => {
  let component: AllNotificationDialog;
  let fixture: ComponentFixture<AllNotificationDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AllNotificationDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AllNotificationDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
