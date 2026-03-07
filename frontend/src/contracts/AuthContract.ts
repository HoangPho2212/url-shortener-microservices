export interface UserLoginContract {
  email: string;
  password: string;
}

export interface UserRegisterContract {
  username: string;
  email: string;
  password: string;
}

export interface AuthResponseContract {
  token: string;
  username: string;
  email: string;
}
