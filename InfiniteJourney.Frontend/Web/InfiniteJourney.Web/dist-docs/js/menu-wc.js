'use strict';

customElements.define('compodoc-menu', class extends HTMLElement {
    constructor() {
        super();
        this.isNormalMode = this.getAttribute('mode') === 'normal';
    }

    connectedCallback() {
        this.render(this.isNormalMode);
    }

    render(isNormalMode) {
        let tp = lithtml.html(`
        <nav>
            <ul class="list">
                <li class="title">
                    <a href="index.html" data-type="index-link">infinite-journey-web documentation</a>
                </li>

                <li class="divider"></li>
                ${ isNormalMode ? `<div id="book-search-input" role="search">
    <input type="text" placeholder="Type to search">
    <button type="button"
        class="search-input-clear"
        aria-label="Clear search"
        data-search-input-clear>&times;</button>
</div>
` : '' }
                <li class="chapter">
                    <a data-type="chapter-link" href="index.html"><span class="icon ion-ios-home"></span>Getting started</a>
                    <ul class="links">
                                <li class="link">
                                    <a href="overview.html" data-type="chapter-link">
                                        <span class="icon ion-ios-keypad"></span>Overview
                                    </a>
                                </li>

                            <li class="link">
                                <a href="index.html" data-type="chapter-link">
                                    <span class="icon ion-ios-paper"></span>
                                        README
                                </a>
                            </li>
                                <li class="link">
                                    <a href="dependencies.html" data-type="chapter-link">
                                        <span class="icon ion-ios-list"></span>Dependencies
                                    </a>
                                </li>
                                <li class="link">
                                    <a href="properties.html" data-type="chapter-link">
                                        <span class="icon ion-ios-apps"></span>Properties
                                    </a>
                                </li>

                    </ul>
                </li>
                    <li class="chapter">
                        <div class="simple menu-toggler" data-bs-toggle="collapse" ${ isNormalMode ? 'data-bs-target="#components-links"' :
                            'data-bs-target="#xs-components-links"' }>
                            <span class="icon ion-md-cog"></span>
                            <span>Components</span>
                            <span class="icon ion-ios-arrow-down"></span>
                        </div>
                        <ul class="links collapse " ${ isNormalMode ? 'id="components-links"' : 'id="xs-components-links"' }>
                            <li class="link">
                                <a href="components/App.html" data-type="entity-link" >App</a>
                            </li>
                            <li class="link">
                                <a href="components/CampaignDetailComponent.html" data-type="entity-link" >CampaignDetailComponent</a>
                            </li>
                            <li class="link">
                                <a href="components/CampaignListComponent.html" data-type="entity-link" >CampaignListComponent</a>
                            </li>
                        </ul>
                    </li>
                    <li class="chapter">
                        <div class="simple menu-toggler" data-bs-toggle="collapse" ${ isNormalMode ? 'data-bs-target="#classes-links"' :
                            'data-bs-target="#xs-classes-links"' }>
                            <span class="icon ion-ios-paper"></span>
                            <span>Classes</span>
                            <span class="icon ion-ios-arrow-down"></span>
                        </div>
                        <ul class="links collapse " ${ isNormalMode ? 'id="classes-links"' : 'id="xs-classes-links"' }>
                            <li class="link">
                                <a href="classes/ActivateCampaignRoute.html" data-type="entity-link" >ActivateCampaignRoute</a>
                            </li>
                            <li class="link">
                                <a href="classes/CampaignDetailDto.html" data-type="entity-link" >CampaignDetailDto</a>
                            </li>
                            <li class="link">
                                <a href="classes/CampaignListItemDto.html" data-type="entity-link" >CampaignListItemDto</a>
                            </li>
                            <li class="link">
                                <a href="classes/CreateCampaignCommand.html" data-type="entity-link" >CreateCampaignCommand</a>
                            </li>
                            <li class="link">
                                <a href="classes/CreateCampaignResultDto.html" data-type="entity-link" >CreateCampaignResultDto</a>
                            </li>
                            <li class="link">
                                <a href="classes/GetCampaignByIdRoute.html" data-type="entity-link" >GetCampaignByIdRoute</a>
                            </li>
                            <li class="link">
                                <a href="classes/GetCampaignsQuery.html" data-type="entity-link" >GetCampaignsQuery</a>
                            </li>
                            <li class="link">
                                <a href="classes/ProblemDetails.html" data-type="entity-link" >ProblemDetails</a>
                            </li>
                            <li class="link">
                                <a href="classes/SwaggerException.html" data-type="entity-link" >SwaggerException</a>
                            </li>
                        </ul>
                    </li>
                        <li class="chapter">
                            <div class="simple menu-toggler" data-bs-toggle="collapse" ${ isNormalMode ? 'data-bs-target="#injectables-links"' :
                                'data-bs-target="#xs-injectables-links"' }>
                                <span class="icon ion-md-arrow-round-down"></span>
                                <span>Injectables</span>
                                <span class="icon ion-ios-arrow-down"></span>
                            </div>
                            <ul class="links collapse " ${ isNormalMode ? 'id="injectables-links"' : 'id="xs-injectables-links"' }>
                                <li class="link">
                                    <a href="injectables/AppConfigService.html" data-type="entity-link" >AppConfigService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/AuthService.html" data-type="entity-link" >AuthService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/CampaignsClient.html" data-type="entity-link" >CampaignsClient</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/TenantContextService.html" data-type="entity-link" >TenantContextService</a>
                                </li>
                            </ul>
                        </li>
                    <li class="chapter">
                        <div class="simple menu-toggler" data-bs-toggle="collapse" ${ isNormalMode ? 'data-bs-target="#interfaces-links"' :
                            'data-bs-target="#xs-interfaces-links"' }>
                            <span class="icon ion-md-information-circle-outline"></span>
                            <span>Interfaces</span>
                            <span class="icon ion-ios-arrow-down"></span>
                        </div>
                        <ul class="links collapse " ${ isNormalMode ? ' id="interfaces-links"' : 'id="xs-interfaces-links"' }>
                            <li class="link">
                                <a href="interfaces/AppConfig.html" data-type="entity-link" >AppConfig</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/IActivateCampaignRoute.html" data-type="entity-link" >IActivateCampaignRoute</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/ICampaignDetailDto.html" data-type="entity-link" >ICampaignDetailDto</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/ICampaignListItemDto.html" data-type="entity-link" >ICampaignListItemDto</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/ICampaignsClient.html" data-type="entity-link" >ICampaignsClient</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/ICreateCampaignCommand.html" data-type="entity-link" >ICreateCampaignCommand</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/ICreateCampaignResultDto.html" data-type="entity-link" >ICreateCampaignResultDto</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/IGetCampaignByIdRoute.html" data-type="entity-link" >IGetCampaignByIdRoute</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/IGetCampaignsQuery.html" data-type="entity-link" >IGetCampaignsQuery</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/IProblemDetails.html" data-type="entity-link" >IProblemDetails</a>
                            </li>
                        </ul>
                    </li>
                    <li class="chapter">
                        <div class="simple menu-toggler" data-bs-toggle="collapse" ${ isNormalMode ? 'data-bs-target="#miscellaneous-links"'
                            : 'data-bs-target="#xs-miscellaneous-links"' }>
                            <span class="icon ion-ios-cube"></span>
                            <span>Miscellaneous</span>
                            <span class="icon ion-ios-arrow-down"></span>
                        </div>
                        <ul class="links collapse " ${ isNormalMode ? 'id="miscellaneous-links"' : 'id="xs-miscellaneous-links"' }>
                            <li class="link">
                                <a href="miscellaneous/enumerations.html" data-type="entity-link">Enums</a>
                            </li>
                            <li class="link">
                                <a href="miscellaneous/functions.html" data-type="entity-link">Functions</a>
                            </li>
                            <li class="link">
                                <a href="miscellaneous/variables.html" data-type="entity-link">Variables</a>
                            </li>
                        </ul>
                    </li>
                        <li class="chapter">
                            <a data-type="chapter-link" href="routes.html"><span class="icon ion-ios-git-branch"></span>Routes</a>
                        </li>
                    <li class="chapter">
                        <a data-type="chapter-link" href="coverage.html"><span class="icon ion-ios-stats"></span>Documentation coverage</a>
                    </li>
                    <li class="divider"></li>
                    <li class="copyright">
                        Documentation generated using <a href="https://compodoc.app/" target="_blank" rel="noopener noreferrer">
                            <img data-src="images/compodoc-vectorise.png" class="img-responsive" data-type="compodoc-logo">
                        </a>
                    </li>
            </ul>
        </nav>
        `);
        this.innerHTML = tp.strings;
    }
});
