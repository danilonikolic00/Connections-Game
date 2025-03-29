import './Group.css'
import { useEffect } from "react";

const Group = ({groupName,solvedTerms}) => {
    useEffect(()=>{
        console.log(solvedTerms);
    },[solvedTerms])
    return (
        <div className="group-container">
            <h1>{groupName}</h1>
            {solvedTerms.map((term) => <p>{term}</p>)}
        </div>
    );
}

export default Group