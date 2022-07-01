import { Component, ViewChild } from '@angular/core';
import { FlowDirective, Transfer } from '@flowjs/ngx-flow';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'web';
  @ViewChild('flowAdvanced')
  public flow!: FlowDirective;

  public autoupload: boolean = false;
  public autoUploadSubscription!: Subscription;
  
  ngAfterViewInit() {
    this.autoUploadSubscription = this.flow.events$.subscribe(event => {
      // to get rid of incorrect `event.type` type you need Typescript 2.8+
      if (this.autoupload && event.type === 'filesSubmitted') {
        this.flow.upload();
      }
    });
  }

  ngOnDestroy() {
    this.autoUploadSubscription?.unsubscribe();
  }

  trackTransfer(idx: number, transfer?: Transfer) {
    return transfer?.id ?? 0;    
  }
}
