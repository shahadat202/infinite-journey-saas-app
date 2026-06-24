export interface AppConfig {
  apiPort: number;
  keycloak: {
    url: string;
    realm: string;
    clientId: string;
  };
}

export const defaultConfig: AppConfig = {
  apiPort: 5274,
  keycloak: {
    url: 'http://localhost:8080',
    realm: 'InfiniteJourney',
    clientId: 'infinite-journey-web'
  }
};
