  'use client'
  import React from 'react';
  import { useDispatch, useSelector } from 'react-redux';
  import { setGameMode, setTimer, closeModal } from '../../store/slices/gameModeSlice';



  export default function GameModeModal() {
    const dispatch = useDispatch();
    const isModalOpen = useSelector((state) => state.gameMode.isModalOpen);
    const gameMode = useSelector((state) => state.gameMode.gameMode);
    const timer = useSelector((state) => state.gameMode.timer);

    if (isModalOpen) return null;

    const handleGameModeClick = (mode) => {
      dispatch(setGameMode(mode));
    };

    const handleTimerChange = (event) => {
      dispatch(setTimer(event.target.value));
    };

    const handlePlayClick = () => {
      console.log(`Selected game mode: ${gameMode}`);
      console.log(`Selected timer: ${timer}`);
      dispatch(closeModal());
    };

    return (
      <div style={modalContentStyles}>
        <h2 style={titleStyles}>Select Game Mode</h2>
        <div style={buttonContainerStyles}>
          <button style={modeButtonStyles} onClick={() => handleGameModeClick('Player')}>👥 Play vs Player</button>
          <button style={modeButtonStyles} onClick={() => handleGameModeClick('Computer')}>🤖 Play vs Computer</button>
          <button style={modeButtonStyles} onClick={() => handleGameModeClick('Friend')}>Play vs Friend</button>
        </div>

        <h3 style={subtitleStyles}>Select Timer</h3>
        <select style={dropdownStyles} value={timer} onChange={handleTimerChange}>
          {['5 min', '10 min', '15 min', '30 min', '60 min', 'No Timer'].map((time) => (
            <option key={time} value={time}>{time}</option>
          ))}
        </select>

        <button style={playButtonStyle} onClick={handlePlayClick}>Play</button>
      </div>
    );
  }

  // Style

  const modalContentStyles = {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    padding: '20px',
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    borderRadius: '15px',
    boxShadow: '0 4px 30px rgba(0, 0, 0, 0.5)',
    backdropFilter: 'blur(10px)',
    border: '1px solid rgba(255, 255, 255, 0.2)',
    width: '100%',
    height: '100%',
    boxSizing: 'border-box',
  };

  const dropdownStyles = {
    padding: '10px',
    fontSize: '16px',
    fontWeight: 'bold',
    color: '#333',
    backgroundColor: '#e0e0e0',
    border: 'none',
    borderRadius: '5px',
    marginBottom: '20px',
    cursor: 'pointer',
  };

  const titleStyles = {
    fontSize: '20px',
    fontWeight: 'bold',
    marginBottom: '20px',
    color: '#fff',
  };

  const buttonContainerStyles = {
    display: 'flex',
    flexDirection: 'column',
    gap: '10px',
    marginBottom: '20px',
  };

  const modeButtonStyles = {
    padding: '10px 20px',
    fontSize: '16px',
    fontWeight: 'bold',
    color: '#fff',
    backgroundColor: '#007bff',
    border: 'none',
    borderRadius: '5px',
    cursor: 'pointer',
  };

  const playButtonStyle = {
    padding: '10px 20px',
    fontSize: '18px',
    fontWeight: 'bold',
    color: '#fff',
    backgroundColor: '#007bff',
    border: 'none',
    borderRadius: '5px',
    cursor: 'pointer',
    width: '80%',
  };

  const subtitleStyles = {
    fontSize: '18px',
    fontWeight: 'bold',
    marginBottom: '10px',
    color: '#fff',
  };
