import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TopServicePartners } from './top-service-partners';

describe('TopServicePartners', () => {
  let component: TopServicePartners;
  let fixture: ComponentFixture<TopServicePartners>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TopServicePartners]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TopServicePartners);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
