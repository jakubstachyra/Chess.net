import RootLayout from '../layout';
import Chessboard from '../components/chessBoard';
import HomeLayout from './layout';

export default function Page() {
  return (
    <HomeLayout>
      <Chessboard/>
    </HomeLayout>
  );
}
