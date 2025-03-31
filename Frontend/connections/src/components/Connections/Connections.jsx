import './Connections.css'
import { useEffect, useState } from "react";
import axios from "axios";
import Group from "../Group/Group";
import Term from "../Term/Term";
import GameModal from "../UI/GameModal"
import logo from "../../../src/assets/unnamed.png"

const Connections = () => {
    const [unsolved, setUnsolved] = useState([]);
    const [groupNames, setGroupNames] = useState([]);
    const [solved, setSolved] = useState([]);
    const [selectedTerms, setSelectedTerms] = useState([])
    const [mistakes, setMistakes] = useState(4);
    const [modalMessage, setModalMessage] = useState(null);

    useEffect(() => {
        getFourGroups();
    }, []);

    useEffect(() => {
        if (mistakes === 0) {
            setModalMessage(`Game over! You found ${groupNames.length} out of 4 groups.`);
        }
        setTimeout(() => {
            if (groupNames.length === 4) {
                setModalMessage("Congratulations! You won!");
            }
        }, 1000);
    }, [mistakes, groupNames.length])

    const shuffleArray = (array) => {
        const shuffled = [...array];
        for (let i = shuffled.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
        }
        return shuffled;
    }

    const getFourGroups = async () => {
        const response = await axios.get('http://localhost:5035/Game/GetTerms');
        const terms = response.data;
        if (terms.length > 0) {
            const shuffledTerms = shuffleArray(terms);
            setUnsolved(shuffledTerms);
        }
    }

    const handleSelectTerm = (term) => {
        setSelectedTerms(prevSelected => {
            if (prevSelected.includes(term)) {
                return prevSelected.filter(t => t !== term);
            }
            if (prevSelected.length < 4) {
                return [...prevSelected, term];
            }
            return prevSelected;
        });
    }

    const handleCheck = async () => {
        if (selectedTerms.length < 4)
            return;
        const response = await axios.get
            (`http://localhost:5035/Game/CheckGuess/${selectedTerms[0]}/${selectedTerms[1]}/${selectedTerms[2]}/${selectedTerms[3]}`);
        if (response.data) {
            setGroupNames(prevGroups => [response.data, ...prevGroups]);
            setSolved(prevSelected => [selectedTerms, ...prevSelected]);
            setUnsolved(prevUnsolved => prevUnsolved.filter(term => !selectedTerms.includes(term)));
            setSelectedTerms([]);
        }
        else {
            setMistakes(prevMistakes => prevMistakes - 1);
        }
    }

    const closeModal = () => {
        setModalMessage(null);
        window.location.reload()
    };

    return (
        <div className="connections-container">
            {modalMessage && (
                <GameModal
                    message={modalMessage}
                    onClose={closeModal}
                    isAlert={false}
                />
            )}
            <div className="logo">
                <img src={logo} />
                <h1>Connections</h1>
            </div>
            {groupNames.map((group, index) => (
                <Group key={group} groupName={group} solvedTerms={solved[index]} />
            ))}
            <div className="terms-container">
                {unsolved.length > 0 && unsolved.map((term) => (
                    <Term
                        key={term}
                        termName={term}
                        isSelected={selectedTerms.includes(term)}
                        onClick={() => handleSelectTerm(term)}
                    />
                ))}
            </div>
            {groupNames.length != 4 && mistakes != 0 &&
                <div className="buttons">
                    <button className="button" onClick={handleCheck}>Check</button>
                    <button className="button" onClick={() => setSelectedTerms([])}>Deselect</button>
                    <button className="button" onClick={() => setUnsolved(shuffleArray(unsolved))}>Shuffle</button>
                </div>}
            <p>Mistakes remaining:</p>
            <div className="mistakes">
                {Array.from({ length: mistakes }).map((_, index) => (
                    <div key={index} className='mistake'></div>
                ))}
            </div>
        </div>
    );
}

export default Connections;