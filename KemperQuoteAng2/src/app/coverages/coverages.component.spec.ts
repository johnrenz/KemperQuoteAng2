import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CoveragesComponent } from './coverages.component';

describe('CoveragesComponent', () => {
  let component: CoveragesComponent;
  let fixture: ComponentFixture<CoveragesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CoveragesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CoveragesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
