import RootLayout from '../layout';
<<<<<<< HEAD
import Chessboard from '../components/chessBoard';

export default function Page() {
=======
import Chessboard from '../components/chessBoard/chessBoard';
import { useDispatch } from 'react-redux';

export default function HomePage() {
>>>>>>> main
  return (
    <RootLayout>
      <Chessboard/>
    </RootLayout>
  );
}
