import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';

@Component({
  selector: 'app-test-error',
  templateUrl: './test-error.component.html',
  styleUrls: ['./test-error.component.sass']
})
export class TestErrorComponent {
  baseUrl = "https://localhost:5001/api/";
  validationErrors: string[] = [];

  constructor(
    private httpClient: HttpClient,
  ) {}

  get404Error(){
    this.httpClient.get(this.baseUrl + "error/not-found").subscribe(({
      next: res => console.log(res),
      error: error => console.log(error)
    }))
  }

  get400Error(){
    this.httpClient.get(this.baseUrl + "error/bad-request").subscribe(({
      next: res => console.log(res),
      error: error => console.log(error)
    }))
  }

  get500Error(){
    this.httpClient.get(this.baseUrl + "error/server-error").subscribe(({
      next: res => console.log(res),
      error: error => console.log(error)
    }))
  }

  get401Error(){
    this.httpClient.get(this.baseUrl + "error/auth").subscribe(({
      next: res => console.log(res),
      error: error => console.log(error)
    }))
  }

  get400ValidationError(){
    this.httpClient.post(this.baseUrl + "account/register", {}).subscribe(({
      next: res => console.log(res),
      error: error => {
        console.log(error)
        this.validationErrors = error;
      }
    }))
  }
}
