import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PolicyInfoComponent } from './policyInfo.component';

describe('PolicyInfoComponent', () => {
  let component: PolicyInfoComponent;
  let fixture: ComponentFixture<PolicyInfoComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PolicyInfoComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PolicyInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
