import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-footer',
  template: `
        <footer class="footer">
            <div class="container-fluid" > 
                <div class="links">
                    <a href="http://wwwint.kemperi.com/AboutKemper/ContactUs" class="footer" target="_blank">Contact Us</a> | 
                    <a href="http://wwwint.kemperi.com/Legal/lp_privacypolicy" class="footer" target="_blank">Privacy Policy</a> | 
                    <a href="http://wwwint.kemperi.com/Legal/lp_termsofuse" class="footer" target="_blank">Terms of Use</a><br /><br />
                </div>
                 <div class="copyright">
                    &copy; 2017 Kemper Direct. All rights reserved.
                </div>
            </div>
        </footer>
  `,
  styleUrls: [ './footer.component.css']
})
export class FooterComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
