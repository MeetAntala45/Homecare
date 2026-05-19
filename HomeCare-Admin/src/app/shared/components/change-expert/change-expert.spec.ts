import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChangeExpert } from './change-expert';

describe('ChangeExpert', () => {
  let component: ChangeExpert;
  let fixture: ComponentFixture<ChangeExpert>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChangeExpert]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ChangeExpert);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
