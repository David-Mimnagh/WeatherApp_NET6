import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AuthorizeService } from '../../api-authorization/authorize.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  public isAuthenticated: Observable<boolean> | undefined;
  public userName!: Observable<string>;

  constructor(private authorizeService: AuthorizeService) { }

  ngOnInit() {
    this.isAuthenticated = this.authorizeService.isAuthenticated();
    this.userName = this.authorizeService.getUser().pipe(map(u =>
      (u && u.name && u.name.substr(0, u.name.indexOf('@'))) ?? ''));
  }
}
