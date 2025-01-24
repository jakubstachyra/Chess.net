'use client';

import React, { useEffect, useState } from 'react';
import FriendsList from '../components/friendsList/friendsList';
import './my-profile.css';
import {
  Container,
  Typography,
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Alert,
  Button,
  TextField,
  Snackbar,
  Alert as MuiAlert,
} from '@mui/material';
import BackgroundUI from 'app/components/backgroundUI/pages';
import CustomDialog from '../components/customDialog/customdialog';
import { useAppSelector } from '../store/hooks';

// Define interfaces for data
interface Ranking {
  Ranking: string;
  RankingInfo: string;
  Points: number | null;
}

interface Friend {
  id: string;
  name: string;
  // Add other relevant fields
}

// Snackbar Alert component
const AlertComponent = React.forwardRef<HTMLDivElement, any>(function Alert(
  props,
  ref
) {
  return <MuiAlert elevation={6} ref={ref} variant="filled" {...props} />;
});

function Rankings() {
  // State Definitions with Explicit Types
  const [rankings, setRankings] = useState<Ranking[] | null>(null);
  const [friends, setFriends] = useState<Friend[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [inviteOpen, setInviteOpen] = useState<boolean>(false);
  const [inviteInput, setInviteInput] = useState<string>('');
  const [confirmButtonColor, setConfirmButtonColor] = useState<'primary' | 'success' | 'darkgreen'>('primary');
  const [inviteError, setInviteError] = useState<string | null>(null);
  const [adminRequestOpen, setAdminRequestOpen] = useState<boolean>(false);
  const [reasonInput, setReasonInput] = useState<string>('');
  const [reasonError, setReasonError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Extract user data from Redux
  const reduxUser = useAppSelector((state) => state.user);
  const currentUser = reduxUser.user;
  const userId = currentUser ? currentUser.userID : null;

  console.log(reduxUser);

  // Fetch Rankings and Friends
  useEffect(() => {
    const fetchRankings = async () => {
      if (!userId) return;
      try {
        const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
        if (!API_BASE_URL) {
          throw new Error('API_BASE_URL is not defined');
        }

        const response = await fetch(`${API_BASE_URL}/rankings/${userId}`);
        if (!response.ok) {
          throw new Error(`HTTP Error: ${response.status}`);
        }
        const data: Ranking[] = await response.json();
        setRankings(data);
      } catch (err) {
        console.error(err);
        setError('Failed to fetch rankings. Please try again later.');
      } finally {
        setLoading(false);
      }
    };

    if (userId) {
      fetchRankings();
      fetchFriends();
    }
  }, [userId]);

  const fetchFriends = async () => {
    if (!userId) return;
    try {
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
      if (!API_BASE_URL) {
        throw new Error('API_BASE_URL is not defined');
      }

      const response = await fetch(`${API_BASE_URL}/friends/${userId}`);
      if (!response.ok) {
        throw new Error(`HTTP Error: ${response.status}`);
      }
      const data: Friend[] = await response.json();
      setFriends(data);
    } catch (err) {
      console.error(err);
      setError('Failed to fetch friends. Please try again later.');
    }
  };
  // Handlers for Admin Request Dialog
  const handleAdminRequestOpen = () => {
    setAdminRequestOpen(true);
    setReasonError(null);
  };

  const handleAdminRequestClose = () => {
    setAdminRequestOpen(false);
    setReasonInput('');
    setReasonError(null);
  };

  const handleAdminRequestSubmit = async () => {
    try {
      if (!reasonInput || reasonInput.trim() === '') {
        setReasonError('Reason is required!');
        return;
      }

      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
      if (!API_BASE_URL) {
        throw new Error('API_BASE_URL is not defined');
      }

      const response = await fetch(`${API_BASE_URL}/admin-requests`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          userId: userId,
          reason: reasonInput.trim(),
        }),
      });

      if (!response.ok) {
        const errorResponse = await response.json();
        setReasonError(errorResponse.error || 'An unknown error occurred.');
        return;
      }

      setConfirmButtonColor('darkgreen');
      setSuccessMessage('Admin request submitted successfully!');
      setTimeout(() => handleAdminRequestClose(), 1000);
    } catch (err) {
      console.error(err);
      setReasonError('Failed to submit the request. Please try again later.');
    }
  };

  // Handlers for Invite Friend Dialog
  const handleInviteOpen = () => {
    setInviteOpen(true);
    setInviteError(null);
  };

  const handleInviteClose = () => {
    setInviteOpen(false);
    setInviteInput('');
    setConfirmButtonColor('primary');
    setInviteError(null);
  };

  const handleSendInvite = async () => {
    try {
      if (!inviteInput || inviteInput.trim() === '') {
        setInviteError('Friend Name is required.');
        return;
      }

      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
      if (!API_BASE_URL) {
        throw new Error('API_BASE_URL is not defined');
      }

      const response = await fetch(`${API_BASE_URL}/friends/${userId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: inviteInput.trim() }), // Assuming the API expects a 'name' field
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`HTTP Error: ${response.status}, Details: ${errorText}`);
      }

      setConfirmButtonColor('success');
      setSuccessMessage('Friend invitation sent successfully!');
      await fetchFriends();
      setTimeout(() => handleInviteClose(), 1000);
    } catch (err) {
      console.error(err);
      setInviteError('Failed to send invite. Please try again.');
    }
  };

  // Render Loading State
  if (loading) {
    return (
      <div style={{ width: '30%', height: '35%' }}>
        <BackgroundUI>
          <div className="loading-spinner bounce-spinner">
            <div className="spinner"></div>
          </div>
        </BackgroundUI>
      </div>
    );
  }

  // Render Error State
  if (error) {
    return (
      <Box style={modalContentStyles}>
        <Alert severity="error">{error}</Alert>
      </Box>
    );
  }

  return (
    <Container component="main" maxWidth="lg">
      <Box style={rankingsContainerStyle}>
        {/* Rankings Table */}
        <Box style={rankingsTableStyle}>
          <BackgroundUI>
            <Typography
              variant="h5"
              style={{
                color: 'white',
                textShadow: '-1px 1px 10px rgba(0, 0, 0, 0.75)',
                marginBottom: '20px',
              }}
            >
              Rankings
            </Typography>
            <TableContainer component={Paper} style={{ backgroundColor: 'rgba(255, 255, 255, 0.1)' }}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell style={tableHeaderStyle}>Category</TableCell>
                    <TableCell style={tableHeaderStyle}>Description</TableCell>
                    <TableCell style={tableHeaderStyle} align="right">
                      Rating
                    </TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {rankings &&
                    rankings.map((ranking) => (
                      <TableRow key={ranking.Ranking}>
                        <TableCell style={tableCellStyle}>{ranking.Ranking}</TableCell>
                        <TableCell style={tableCellStyle}>{ranking.RankingInfo}</TableCell>
                        <TableCell style={tableCellStyle} align="right">
                          {ranking.Points !== null ? ranking.Points : 'N/A'}
                        </TableCell>
                      </TableRow>
                    ))}
                </TableBody>
              </Table>
            </TableContainer>
            <Button
              variant="contained"
              color="secondary"
              style={{ marginTop: '20px' }}
              onClick={handleAdminRequestOpen}
            >
              Become Admin
            </Button>
          </BackgroundUI>
        </Box>

        {/* Friends List */}
        <Box style={friendsListStyle}>
          <Typography
            variant="h6"
            style={{
              color: 'white',
              textShadow: '-1px 1px 10px rgba(0, 0, 0, 0.75)',
              marginBottom: '20px',
            }}
          >
            Friends
          </Typography>
          <FriendsList friends={friends} onRemoveFriend={() => fetchFriends()} />
          <Button
            variant="contained"
            color="primary"
            onClick={handleInviteOpen}
            style={{
              margin: '20px auto 0',
              display: 'block',
            }}
          >
            Invite Friend
          </Button>
        </Box>
      </Box>

      {/* Invite Friend Dialog */}
      <CustomDialog
        open={inviteOpen}
        onClose={handleInviteClose}
        title="Invite Friend"
        content={
          <>
            <TextField
              autoFocus
              margin="dense"
              label="Friend Name"
              type="text"
              fullWidth
              value={inviteInput}
              error={inviteError !== null}
              helperText={inviteError || ''}
              onChange={(e) => {
                setInviteInput(e.target.value);
                setInviteError(null);
              }}
              sx={{
                '& .MuiInputBase-root': { color: 'white' },
                '& .MuiInputLabel-root': { color: 'white' },
                '& .MuiOutlinedInput-root.Mui-error': { borderColor: 'red' },
              }}
            />
          </>
        }
        actions={
          <>
            <Button
              onClick={handleSendInvite}
              color={confirmButtonColor === 'success' ? 'success' : 'primary'}
              variant="contained"
              sx={{
                margin: '0 auto',
                display: 'block',
                backgroundColor: confirmButtonColor === 'success' ? 'green' : 'lightgreen',
                color: confirmButtonColor === 'success' ? 'white' : 'grey',
              }}
            >
              {confirmButtonColor === 'success' ? 'Sent!' : 'Send Invite'}
            </Button>
          </>
        }
      />

      {/* Become Admin Dialog */}
      <CustomDialog
        open={adminRequestOpen}
        onClose={handleAdminRequestClose}
        title="Become Admin"
        content={
          <>
            <Typography variant="body1" style={{ color: 'white', marginBottom: '10px' }}>
              Please provide a reason for your request to become an admin:
            </Typography>
            <TextField
              autoFocus
              margin="dense"
              label="Reason"
              type="text"
              fullWidth
              value={reasonInput}
              error={reasonError !== null}
              helperText={reasonError || ''}
              onChange={(e) => {
                setReasonInput(e.target.value);
                setReasonError(null);
              }}
              sx={{
                '& .MuiInputBase-root': { color: 'white' },
                '& .MuiInputLabel-root': { color: 'white' },
                '& .MuiOutlinedInput-root.Mui-error': {
                  borderColor: 'red', // Red border on error
                },
              }}
            />
          </>
        }
        actions={
          <>
            <Button
              onClick={handleAdminRequestSubmit}
              variant="contained"
              sx={{
                backgroundColor: confirmButtonColor === 'darkgreen' ? 'darkgreen' : 'lightgreen',
                color: confirmButtonColor === 'darkgreen' ? 'white' : 'grey',
              }}
            >
              {confirmButtonColor === 'darkgreen' ? 'Request Sent' : 'Submit'}
            </Button>
            <Button
              onClick={handleAdminRequestClose}
              color="primary"
              variant="outlined"
              sx={{ color: 'white', borderColor: 'white' }}
            >
              Cancel
            </Button>
          </>
        }
      />

      {/* Success Snackbar */}
      <Snackbar
        open={successMessage !== null}
        autoHideDuration={6000}
        onClose={() => setSuccessMessage(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <AlertComponent onClose={() => setSuccessMessage(null)} severity="success" sx={{ width: '100%' }}>
          {successMessage}
        </AlertComponent>
      </Snackbar>
    </Container>
  );
}

export default Rankings;

// Style Definitions with Explicit Typing
const modalContentStyles: React.CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  padding: '20px',
  backgroundColor: 'rgba(255, 255, 255, 0.1)',
  borderRadius: '15px',
  boxShadow: '0 4px 30px rgba(0, 0, 0, 0.5)',
  backdropFilter: 'blur(10px)',
  border: '1px solid rgba(255, 255, 255, 0.2)',
};

const rankingsContainerStyle: React.CSSProperties = {
  display: 'flex',
  justifyContent: 'space-between',
  gap: '20px',
};

const rankingsTableStyle: React.CSSProperties = {
  flex: 2,
};

const friendsListStyle: React.CSSProperties = {
  flex: 1,
  backgroundColor: 'rgba(255, 255, 255, 0.1)',
  borderRadius: '15px',
  padding: '20px',
  boxShadow: '0 4px 30px rgba(0, 0, 0, 0.5)',
  backdropFilter: 'blur(10px)',
  border: '1px solid rgba(255, 255, 255, 0.2)',
};

const tableHeaderStyle: React.CSSProperties = {
  color: 'white',
  fontWeight: 'bold',
  textShadow: '-1px 1px 10px rgba(0, 0, 0, 0.75)',
};

const tableCellStyle: React.CSSProperties = {
  color: 'white',
  textShadow: '-1px 1px 5px rgba(0, 0, 0, 0.5)',
};
