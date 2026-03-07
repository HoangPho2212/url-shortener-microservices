export interface AuthViewState {
  loading: boolean;
  error: string | null;
  success: boolean;
}

export const createInitialAuthState = (): AuthViewState => ({
  loading: false,
  error: null,
  success: false,
});
