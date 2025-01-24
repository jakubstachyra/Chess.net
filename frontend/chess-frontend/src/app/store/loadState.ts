export interface UserState {
    user: User | null;
    token: string | null;
  }
  
  export interface User {
    id: string;
    name: string;
    email: string;
  }
  
  export const loadUserState = (): UserState => {
    try {
      const serializedUser = localStorage.getItem('user');
      const serializedToken = localStorage.getItem('token');
  
      return {
        user: serializedUser ? JSON.parse(serializedUser) : null,
        token: serializedToken || null,
      };
    } catch (err) {
      console.warn('Nie udało się wczytać stanu użytkownika z localStorage:', err);
      return {
        user: null,
        token: null,
      };
    }
  };
  