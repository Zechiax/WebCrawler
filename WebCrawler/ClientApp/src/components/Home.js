import React, { Component } from "react";
import Container from "react-bootstrap/Container";
import Row from "react-bootstrap/Row";
import Stack from "react-bootstrap/Stack";
import Button from "react-bootstrap/Button";
import { NavLink } from "react-router-dom";
import { CreateWebsiteRecordModalWindow } from "./CreateWebsiteRecordModalWindow";
import PaginatedView from "./PaginatedView";

export class Home extends Component {
  static displayName = Home.name;

  constructor(props) {
    super(props);

    this.state = {
      isCreateWebsiteRecordModalShown: false,
      selectedGraphsIds: [6, 7, 8], // TODO
    };
  }

  onCreateWebsiteRecordModalClose = () =>
    this.setState({ isCreateWebsiteRecordModalShown: false });

  render() {
    return (
      <>
        <Container fluid="md" style={{ display: 'flex', flexDirection: 'column'}}>
            <div style={{ flex: '1', marginBottom: '1rem' }}>
                <PaginatedView />
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
                <Button
                    variant="primary"
                    onClick={() =>
                        this.setState({ isCreateWebsiteRecordModalShown: true })
                    }
                >
                    Create new website record
                </Button>
            </div>
        </Container>

        <CreateWebsiteRecordModalWindow
          show={this.state.isCreateWebsiteRecordModalShown}
          urlPresetValue="https://cs.wikipedia.org/wiki/Hlavn%C3%AD_strana"
          labelPresetValue="Wikipedie"
          passCreatedRecordId={(id) => {}}
          onClose={this.onCreateWebsiteRecordModalClose}
        />
      </>
    );
  }
}
