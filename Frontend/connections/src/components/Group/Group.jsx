import './Group.css'

const Group = ({ groupName, solvedTerms }) => {
    return (
        <div className="group-container">
            <h1>{groupName}</h1>
            {solvedTerms.map((term) => <p key={term}>{term}</p>)}
        </div>
    );
}

export default Group