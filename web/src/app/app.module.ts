import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './app.component';
import { NgxFlowModule, FlowInjectionToken } from '@flowjs/ngx-flow';
import * as Flow from '@flowjs/flow.js';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    NgxFlowModule,
    FormsModule 
  ],
  providers: [
    {
      provide: FlowInjectionToken,
      useValue: Flow
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
