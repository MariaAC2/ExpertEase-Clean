import { Component } from '@angular/core';
import {FormsModule} from "@angular/forms";
import {NgForOf, NgIf} from "@angular/common";
import {SearchInputComponent} from "../../shared/search-input/search-input.component";
import {SpecialistCardComponent} from "../../shared/specialist-card/specialist-card.component";

@Component({
  selector: 'app-about',
  standalone: true,
    imports: [],
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.scss']
})
export class AboutComponent {

}
