import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyService } from './my-service';

describe('MyJobs', () => {
  let component: MyService;
  let fixture: ComponentFixture<MyService>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyService]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyService);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
