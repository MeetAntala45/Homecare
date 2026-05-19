import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EdicationEntry } from './edication-entry';

describe('EdicationEntry', () => {
  let component: EdicationEntry;
  let fixture: ComponentFixture<EdicationEntry>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EdicationEntry]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EdicationEntry);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
