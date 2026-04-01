import { UrlRecord } from '../contracts/UrlContract';

export interface DashboardViewState {
  urls: UrlRecord[];
  isLoading: boolean;
  isShortening: boolean;
  error: string | null;
  newUrl: string;
}

export const initialDashboardViewState: DashboardViewState = {
  urls: [],
  isLoading: false,
  isShortening: false,
  error: null,
  newUrl: '',
};
