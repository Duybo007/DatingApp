import { Component } from '@angular/core';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { HasRoleDirective } from '../../_directives/has-role.directive';
import { UserManagermentComponent } from "../user-managerment/user-managerment.component";
import { PhotoManagermentComponent } from "../photo-managerment/photo-managerment.component";

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [TabsModule, HasRoleDirective, UserManagermentComponent, PhotoManagermentComponent],
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.css'
})
export class AdminPanelComponent {

}
