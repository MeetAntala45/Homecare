import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LanguageEntry } from './language-entry';

describe('LanguageEntry', () => {
  let component: LanguageEntry;
  let fixture: ComponentFixture<LanguageEntry>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LanguageEntry]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LanguageEntry);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
